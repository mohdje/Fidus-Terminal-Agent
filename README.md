
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
- **Multiple Agents**: Create, configure, and manage multiple assistant agents with distinct personalities and behaviors.
- **Safe by Design**: Built-in safety checks to prevent destructive operations.
- **Customizable**: Easily switch between different AI providers and models.

## How It Works

1. Start Fidus in your terminal.
2. Interact with the assistant using natural language.
3. Fidus interprets your requests, consults the connected AI model, and executes safe commands on your behalf.
4. Fidus can also do research on internet when up to date data are needed.

## Supported Providers
- Hugging Face
- Cerebras
- Google
- OpenAI
- ...and any other compatible LLM API

## Example Usage

When using the first time execute the following to setup your terminal agent
```bash
fidus --setup
```

Then to use your terminal agent
```bash
fidus
```

Once started, just type your request:

```
> List all files modified today
```

Or ask for script, explanations, or automation help:

```
> How do I find and delete all .tmp files?
```

## Multiple Agents
Fidus now supports creating and managing multiple agents, so you can run different assistants for different roles or workflows.

- Create agents with custom names, personalities, and behavior settings.
- Switch between agents depending on the task at hand.
- List available agents and remove agents you no longer need.

Example commands:

```bash
# to setup an agent, CLI app will then guide you to set each option for you agent.
fidus --setup -a "java-expert"

# to start working with your agent.
fidus -a "java-expert"

# to list all available agents
fidus --list-agents

#to remove an agent
fidus --remove-agent -a "java-expert"
```

Each agent can have its own prompts, default model, and usage style, making Fidus more flexible for complex terminal workflows.

## Safety
Fidus will never run destructive commands without explicit confirmation and is designed to keep your system safe.

## How to install it ?

Download the installation script in Release section and execute it. It will download the zip, extract the deb package, install it and create a link in /usr/bin so you can use fidus command everywhere.

Or you can do the installation manually:

- Download the zip in Releases section, extract the .deb package and install it:

```bash
dpkg -i fidus.deb
```
- Create a link to the fidus binary so you can launch Fidus from anywhere

```bash
sudo ln -s /usr/bin/fidusterminal/fidus /usr/bin/fidus
```

## License
MIT

---

Created with ❤️ for Linux power users and AI enthusiasts.