
<p align="center">
	<img src="./fidus-logo.svg" alt="Fidus Logo" width="128" height="128"/>
</p>

# Fidus: AI-Powered Linux CLI Assistant

Fidus is a powerful command-line assistant for Linux, designed to help you accomplish tasks directly from your terminal using the latest AI models.

## Features

- **Terminal-native**: Runs entirely in your terminal, no GUI required.
- **AI-Powered**: Connects to your favorite Large Language Models (LLMs) including Hugging Face, Cerebras, Google, OpenAI, and more.
- **Flexible**: Supports both single-line commands and multi-line scripts.
- **Smart Automation**: Execute shell commands, automate workflows, and get intelligent help for coding, system operations, and more.
- **Safe by Design**: Built-in safety checks to prevent destructive operations.
- **Customizable**: Easily switch between different AI providers and models.

## How It Works

1. Start Fidus in your terminal.
2. Interact with the assistant using natural language or shell commands.
3. Fidus interprets your requests, consults the connected AI model, and executes safe commands on your behalf.

## Supported Providers
- Hugging Face
- Cerebras
- Google
- OpenAI
- ...and any other compatible LLM API

## Example Usage

If no model configured, set parameters to determine which model to use
```bash
fidus -i huggingface -m gpt-oss-20b -a <your_token>
```

Start Fidus:
```bash
fidus
```

Once started, just type your request:

```
> List all files modified today
```

Or ask for code, explanations, or automation help:

```
> How do I find and delete all .tmp files?
```

## Safety
Fidus will never run destructive commands without explicit confirmation and is designed to keep your system safe.

## How to install it ?
Download the zip in Releases section, extract the .deb package and install it:

```bash
dpkg -i fidus.deb
```
Create a link to the fidus binary so you can launch Fidus from anywhere

```bash
sudo ln -s /usr/bin/fidusterminal/fidus /usr/bin/fidus
```
## License
MIT

---

Created with ❤️ for Linux power users and AI enthusiasts.