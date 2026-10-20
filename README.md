
<p align="center">
	<img src="./fidus-logo.svg" alt="Fidus Logo" width="128" height="128"/>
</p>

# Fidus: AI-Powered Cross-Platform CLI Assistant

Fidus is a command-line assistant for Windows, macOS, and Linux. Use natural language in your terminal to get help with coding, system operations, research, and automation, powered by the latest AI models.

Fidus supports:

- Windows
- macOS
- Linux

## Features

- **Terminal-native**: Runs directly in your terminal on Windows, macOS, and Linux; no GUI required.
- **AI-Powered**: Connects to your favorite Large Language Models (LLMs) including Hugging Face, Cerebras, Google, OpenAI, and more.
- **Flexible**: Supports both single-line commands and multi-line scripts.
- **Smart Automation**: Execute shell commands, automate workflows, and get intelligent help for coding, system operations, and more.
- **Multiple Agents**: Create, configure, and manage multiple assistant agents with distinct personalities and behaviors.
- **Saved Context History**: Conversation context and a readable transcript are saved automatically for each agent, so you can continue a previous session.
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

## Save and Resume Context

Fidus automatically saves each agent's conversation context and transcript as you chat. Resume the previous session for the default `terminal` agent with:

```bash
fidus --resume
```

Use `-r` as a shorthand. To resume a named agent's session, specify it with `-a`:

```bash
fidus -r -a "java-expert"
```

The saved conversation is restored as context for the assistant, and the transcript from the previous session is shown when the session starts.

## Updating Fidus

Check for and install the latest stable release with:

```bash
fidus --update
```

Use `-u` as a shorthand. When an update is available, Fidus asks for administrator authorization, starts the installer for your operating system, and exits so the installer can replace the CLI.

## Safety
Fidus will never run destructive commands without explicit confirmation and is designed to keep your system safe.

## How to install it?

Download the installation script corresponding to your OS from the Releases section or from https://fidus-terminal.vercel.app/ and run it.

## License
MIT

---

Created with ❤️ for power users and AI enthusiasts across platforms.