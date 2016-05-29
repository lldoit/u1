local person_pb = require "protol/person_pb"
local ui_messagebox = require "ui/ui_messagebox"
local ui_teammanage_scene = require "ui/ui_teammanage_scene"

local ui_mainmenu_cellbtn = class(ui_session)
function ui_mainmenu_cellbtn:init()

end

function ui_mainmenu_cellbtn:refresh()

end

local ui_mainmenu_scene = class(ui_session)

function ui_mainmenu_scene.show_me()
    local sd = ui_session_data(ui_session_type.NORMAL, ui_session_id.UI_MAINMENU_SCENE, true)
    local ui_manager = fw_facade:instance():get_mgr(mgr_name.UI_MGR)
    if ui_manager ~= nil then
        ui_manager:instance():show_session(ui_mainmenu_scene(sd))
    end
end

function ui_mainmenu_scene:init(session_data)
    self._base.init(self, session_data)
    self.session_id = ui_session_id.UI_MAINMENU_SCENE
end

function ui_mainmenu_scene:on_post_load()
  local _view_ctrl = self.mainview:GetController("c1");
  
  local btn_Back = self.mainview:GetChild("btn_Back")
  self.lua_behaviour:AddClick(btn_Back, function(go)
      _view_ctrl.selectedIndex = 0;
      btn_Back.visible = false;
  end)
  
  local btn_Button = self.mainview:GetChild("btn_Button")
  self.lua_behaviour:AddClick(btn_Button, function(go)
      _view_ctrl.selectedIndex = 1;
      btn_Back.visible = true;
  end)
end

function ui_mainmenu_scene:reset_window(args)
  
end

function ui_mainmenu_scene:on_pre_destroy()

end

return ui_mainmenu_scene
