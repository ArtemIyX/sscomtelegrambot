# SS.LV - Telegram Bot
<img width="1156" height="1146" alt="image" src="https://github.com/user-attachments/assets/aca10e23-c13c-468b-bd2c-96319b7da722" />

A Telegram bot for monitoring and filtering apartment rentals in Riga from the ss.lv (ss.com) website. 

The bot fetches data directly from the site, stores it in a database, and allows users to apply filters right within Telegram chats.

## Features
- Data Fetching: Asynchronously parses rental listings from ss.lv using HTML parsing.
- Filtering: Use the /filter command in Telegram to search rentals based on criteria (e.g., price, rooms, location).
- Refresh: Manually refresh the database with the latest listings via /refresh.

## Project Structure

The solution is divided into several projects for better organization and maintainability:

- consoletest: A console application for debugging and testing purposes (e.g., manual parsing or data inspection).
- SS.Data: Contains data models (e.g., entities for rentals like flats, prices, locations) and database-related logic (e.g., EF Core contexts if applicable).
- SS.Parser: Handles HTML parsing of ss.lv pages and asynchronous batch fetching of rental listings.
- SS.Telegram: The core Telegram bot implementation. Includes:
    - Bot hosting as a .NET hosted service.
    - Integration with the ss.lv singleton service (via dependency injection container).
    - Command handlers (e.g., /refresh, /filter).
 
## Prerequisites
- .NET SDK (version 9.0 or later recommended).
- A Telegram Bot Token (obtain from @BotFather on Telegram).

## Installation and Setup

1. Clone or Download the Repository
2. Build the Solution
3. Download [Release](https://github.com/ArtemIyX/sscomtelegrambot/releases) (Alternative)
4. Configure appsettings.json:
   ```json
   {
    "TelegramBotToken": "YOUR_BOT_TOKEN_HERE"
   }
   ```
5. Run the bot

## Usage in Telegram
- **/refresh**: Triggers a full refresh of the database by fetching and parsing the latest rentals from ss.lv for Riga. This updates the local cache/DB.
- **/filter**: Apply filters to search rentals. Example usage:
  ``/filter 100-400;1,2;30-60;plyavnieki,purvciems,darzciems``

## License
This project is open-source under the MIT License. See [LICENSE](https://github.com/ArtemIyX/sscomtelegrambot/blob/main/LICENSE) for details.
