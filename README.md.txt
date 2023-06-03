Project have one Scene in Assets/Game/_Scenes
At Start (in Demo Mode) will be opened the Trader Table  between Player and Vendor (between them in the middle the Player Wallet)
You can use Drag&Drop to Sell items between these Inventories

Features:
- The Demo Mode can be tuen off by unactivate the GameObject "DEBUG: Emulate Selection of Vendor"
- Use GameObject Inventory Manager (from [GAME SYSTEMS]) you can select a Vendor from the list of Vendors and open Trader Table between them and Player
- You can create any number Vendor by dublicate coresponding objects (class VendorSystemInstaller) in [GAME SYSTEMS]
- GameObjects in [GAME_SYSTEMS] (class VendorSystemInstaller and PlayerSystemInstaller) give a possibility to manage a corespondent Wallets in PlayMode
- SO from Assets/Game/GameEngine/Inventory/Content give a possibility to change the initial inventory of Player and Vendors, and change the Dimention of Trader Table (number Items, spacing between them in reasonable limits). Initialy Trader Tables did for 1920 х 1080 (and set AutoSizing). Other dimentions not tested yet.

Specific Stack:
Zenject & ODIN

WalletStorage use the MVO pattern
Trader Tables it's a some mix of MVA (Active/Passive) patterns