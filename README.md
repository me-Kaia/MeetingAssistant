# AI Meeting Assistant

一个基于 WPF 的桌面会议助手：录音 → 本地 Whisper 语音转文字 → 调用 LLM API 生成会议纪要，端到端全部跑通。

## 项目背景

日常工作中经常需要把会议口头讨论内容整理成书面纪要，这个过程通常依赖人工记录，效率不高。这个项目尝试用本地语音识别 + LLM 总结的方式，实现"录完即有纪要"的小工具，同时也是一次完整的端到端 AI 应用开发实践：从录音采集、本地模型推理，到云端大模型调用，覆盖了语音、Agent 应用的核心链路。

## 功能

1. 点击按钮开始/停止录音，音频以 WAV 格式实时写入本地文件
2. 停止录音后，自动调用本地部署的 Whisper 模型（whisper.cpp）完成语音转文字
3. 转写结果自动传给 LLM（DeepSeek API），生成结构化的会议要点总结
4. 转写文本与 AI 总结分别展示在界面上

## 技术栈

- **UI / 客户端**：WPF（C# / .NET）
- **录音**：NAudio（WaveInEvent + WaveFileWriter）
- **语音转文字**：whisper.cpp 本地部署（通过 Process 调用可执行文件，模型为 ggml-base）
- **AI 总结**：DeepSeek API（HttpClient + System.Text.Json）

## 架构流程

```
[开始录音] → NAudio 采集音频 (16kHz/单声道 WAV)
     ↓
[停止录音] → 等待文件写入完成（异步信号量避免竞态）
     ↓
whisper.cpp 本地推理 → 输出转写文本
     ↓
调用 LLM API（DeepSeek）→ 生成会议纪要要点
     ↓
界面展示：转写原文 + AI 总结
```

## 开发过程中的问题排查记录

这个项目从界面骨架到完整跑通，过程中遇到了几个比较典型的工程问题，记录下来：

### 1. 路径硬编码，项目不可移植
最初录音文件路径、whisper.cpp 可执行文件路径都是写死的绝对路径（如 `D:\whisper\...`），换一台机器就无法运行。改用 `Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ...)` 基于程序运行目录动态拼接路径，并将 whisper.cpp 所需文件放入项目内的子文件夹，通过项目文件配置自动复制到输出目录。

### 2. whisper.cpp 运行缺少依赖 DLL
只复制了 `whisper-cli.exe` 和模型文件，运行时报 `找不到 ggml.dll`。whisper.cpp 实际运行依赖一整套 DLL（如 `ggml-cpu-*.dll` 等），需要将 Release 目录下的全部文件一并复制，而不是只挑主程序和模型文件。

### 3. 录音格式与 whisper.cpp 要求不匹配
NAudio 默认录音格式（44100Hz / 立体声）与 whisper.cpp 期望的 16kHz 单声道不一致，导致 `failed to read audio file`。显式指定 `WaveFormat(16000, 1)` 解决。

### 4. 异步竞态：录音文件未写完就被读取
`StopRecording()` 触发停止后，音频文件的实际关闭（`writer.Dispose()`）发生在异步事件回调中，紧接着调用转写会读到不完整的文件。引入 `ManualResetEventSlim` 作为完成信号，`StopRecording` 等待文件真正写入完成后再返回，避免竞态。

### 5. 死锁：同步等待阻塞 UI 线程
上一步的等待逻辑最初直接在 UI 线程同步调用 `.Wait()`，导致界面卡死——因为 NAudio 内部事件的触发也依赖消息循环，两者互相等待造成死锁。将等待逻辑放入 `Task.Run` 异步执行，`StopRecording` 改为 `async Task` 方法，避免阻塞 UI 线程。

### 6. API Key 安全与网络环境问题
最初将 API Key 硬编码在代码中，存在泄露风险，改为从环境变量读取。调用 API 时一度遇到 SSL 连接失败，排查后确认是环境变量设置后未重启开发环境导致读取为空，重启后恢复正常。

## 已知待改进

- Whisper 模型路径与可执行文件依赖当前基于本地环境，尚未做跨平台/跨环境的自动化打包
- 暂无异常场景下的重试机制（如网络波动导致的 API 调用失败）
- UI 较为简陋，仅作为功能验证用途

## 使用方法

1. 准备好 whisper.cpp 可执行文件、依赖 DLL 及模型文件，放入项目 `whisper` 文件夹
2. 设置环境变量 `DEEPSEEK_API_KEY` 为你自己的 DeepSeek API Key
3. 编译运行，点击"开始录音"进行测试

---

*本项目为个人技术实践项目，记录了完整的端到端开发与问题排查过程。*
