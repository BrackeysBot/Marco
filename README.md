<h1 align="center">Marco</h1>
<p align="center"><i>Marco? Polo! A Discord bot for creating automated responses (aka macros / custom commands).</i></p>
<p align="center">
<a href="https://github.com/BrackeysBot/Marco/releases"><img src="https://img.shields.io/github/v/release/BrackeysBot/Marco?include_prereleases"></a>
<a href="https://github.com/BrackeysBot/Marco/actions?query=workflow%3A%22.NET%22"><img src="https://img.shields.io/github/workflow/status/BrackeysBot/Marco/.NET" alt="GitHub Workflow Status" title="GitHub Workflow Status"></a>
<a href="https://github.com/BrackeysBot/Marco/issues"><img src="https://img.shields.io/github/issues/BrackeysBot/Marco" alt="GitHub Issues" title="GitHub Issues"></a>
<a href="https://github.com/BrackeysBot/Marco/blob/main/LICENSE.md"><img src="https://img.shields.io/github/license/BrackeysBot/Marco" alt="MIT License" title="MIT License"></a>
</p>

## About
Marco is a Discord bot which can respond to macros in a Discord server, allowing you to define macros for specific channels or in all channels.

## Installing and configuring Marco 
Marco runs in a Docker container, and there is a [docker-compose.yaml](docker-compose.yaml) file which simplifies this process.

### Clone the repository
To start off, clone the repository into your desired directory:
```bash
git clone https://github.com/BrackeysBot/Marco.git
```
Step into the Marco directory using `cd Marco`, and continue with the steps below.

### Setting things up
The bot's token is passed to the container using the `DISCORD_TOKEN` environment variable. Create a file named `.env`, and add the following line:
```
DISCORD_TOKEN=your_token_here
```

Two directories are required to exist for Docker compose to mount as container volumes, `data` and `logs`:
```bash
mkdir data
mkdir logs
```
Copy the example `config.example.json` to `data/config.json`, and assign the necessary config keys. Below is breakdown of the config.json layout:
```json
{
  "GUILD_ID": {
    "cooldown": /* The cooldown, in milliseconds, between duplicate macro usages. Defaults to 5000 */,
    "prefix": /* The macro prefix. Defaults to [] */,
    "reactions": {
      "successReaction": /* The reaction the bot will give when a known macro is used. Defaults to ✅ (:white_check_mark:) */,
      "unknownReaction": /* The reaction the bot will give when an unknown macro is used. Defaults to null. */,
      "cooldownReaction": /* The reaction the bot will give when the same macro is used in quick succession. ⏳ (:hourglass_flowing_sand:) */
    }
  }
}
```
The `logs` directory is used to store logs in a format similar to that of a Minecraft server. `latest.log` will contain the log for the current day and current execution. All past logs are archived.

The `data` directory is used to store persistent state of the bot, such as config values and the infraction database.

### Launch Marco
To launch Marco, simply run the following commands:
```bash
sudo docker-compose build
sudo docker-compose up --detach
```

## Updating Marco
To update Marco, simply pull the latest changes from the repo and restart the container:
```bash
git pull
sudo docker-compose stop
sudo docker-compose build
sudo docker-compose up --detach
```

## Using Marco
For further usage breakdown and explanation of commands, see [USAGE.md](USAGE.md).

## License
This bot is under the [MIT License](LICENSE.md).

## Disclaimer
This bot is tailored for use within the [Brackeys Discord server](https://discord.gg/brackeys). While this bot is open source and you are free to use it in your own servers, you accept responsibility for any mishaps which may arise from the use of this software. Use at your own risk.
