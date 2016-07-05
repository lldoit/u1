require("common/init")
require("ui/init")
require("eventsystem/init")
require("framework/init")

string_table = require("globalization/zh/string_table")

UGameObject = UnityEngine.GameObject
UObject = UnityEngine.Object

UNetworkManager = LuaFramework.LuaHelper.GetNetManager()
UGameResFactory = LuaFramework.LuaHelper.GetGameResFactory()
