# Elite Dangerous to SPAD.neXt Integration Script

## Purpose
This script bridges the gap between **Elite Dangerous** and **SPAD.neXt** by automatically importing real-time game state data into SPAD's local variable environment. It eliminates the need for external middleware to read game status, allowing you to create highly responsive UI automations (Stream Decks, physical gauges) directly within SPAD.neXt rules based on live game data.
See https://doc.elitedangereuse.fr/Status%20File/ for more details on the state data provided by Elite Dangerous.

## Highlights
- **Real-Time Monitoring**: Automatically detects updates to the Elite Dangerous `Status.json` file and processes new values immediately.
- **Seamless Integration**: Maps game state variables directly into SPAD.neXt Data Variables using a distinct prefix (`ED2SPADNEXT`).
- **Zero-Touch Operation**: Designed to run passively in the background; no manual triggers are required for data ingestion.
- **Flexible Configuration**: Supports custom status file paths via an optional JSON override file, useful for multi-location setups or testing.

## Installation:
1.  **Place in SPAD.neXt Addons directory**: Unpack the DLL files into the SPAD.neXt Addons directory, normally located in `%userprofile%\Documents\SPAD.neXt\Addons`. Create the Addons directory if it does not exist. SPAD.neXt will automatically load the assembly on startup and initialize the script.

## Usage
1.  **Understand Data Mapping**: The script writes data to SPAD.neXt Variables in the `LOCAL` session scope. All variables created by this add-on are prefixed with `ED2SPADNEXT_`.
    -   *Example*: If your game's `Status.json` indicates that the Docked state is set, it will appear in SPAD as `LOCAL:ED2SPADNEXT_EDFLAGS_DOCKED` with a value of `1`.
2.  **Monitor Game State**: Once installed, open the SPAD.neXt variables panel or create rules to inspect the new variables. You can now use standard SPAD Logic and triggers based on this imported data (e.g., "IF `LOCAL:ED2SPADNEXT_EDFLAGS_LANDING_GEAR_DOWN` is True, THEN turn on a physical indicator light").
3.  **Advanced Configuration**: By default, the script monitors `%userprofile%\Saved Games\Frontier Developments\Elite Dangerous\Status.json`. To change this location:
    -   Create a `location_override.json` file in the same directory as the script DLL.
    -   Edit it to specify your custom path:
```json
{
  "StatusFilePathOverride": "C:\\CustomPath\\To\\Elite Dangerous\\Status.json"
}
```

## Limitations
The script reads data exclusively from Elite Dangerous; it cannot send commands back into the game (use VJoy or similar middleware for that). Additionally, the variable mapping depends entirely on the structure of Frontier's `Status.json`; if they change the export format in future game updates, this script may require adjustments to remain compatible.
The code was tested and developed on Windows 11 and might not work on other operating systems.
