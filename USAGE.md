# Macros

Slash commands are **NOT** used to invoke macros, as registering slash commands is a slow process and can take up to an hour to refresh.
A standard command prefix is used instead, which by default is `[]`, but this can be configured in config.json (see [README.md](README.md) for details.)

# Slash Commands

Below is an outline of every slash command currently implemented in Marco, along with their descriptions and parameters.

### `/addmacro`

Add a new macro. This displays a modal where a lengthy response can be written.

| Parameter | Required | Type                  | Description                                        |
|:----------|:---------|:----------------------|:---------------------------------------------------|
| name      | ✅ Yes    | String                | The name of the new macro.                         |
| channel   | ❌ No     | Channel mention or ID | The channel to which the macro will be restricted. |

### `/editmacro`

Edits an existing macro. This displays a modal where the channel ID and response can be modified.

| Parameter | Required | Type   | Description                      |
|:----------|:---------|:-------|:---------------------------------|
| name      | ✅ Yes    | String | The name of the macro to delete. |

### `/deletemacro`

Deletes an existing macro.

| Parameter | Required | Type                  | Description                      |
|:----------|:---------|:----------------------|:---------------------------------|
| name      | ✅ Yes    | String                | The name of the macro to delete. |

### `/listmacros`

Lists all available macros.

| Parameter | Required | Type | Description |
|:----------|:---------|:-----|:------------|
| -         | -        | -    | -           |

# Ephemeral responses

None of the commands in Marco respond ephemerally.
