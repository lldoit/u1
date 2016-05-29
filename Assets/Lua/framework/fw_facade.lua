local fw_controller = require "framework/fw_controller"
local fw_startcommand = require "framework/fw_startcommand"

fw_facade = class()

function fw_facade:init()
    self.controller = fw_controller:instance()
    self.managers = { }
    self:register_command(noti_const.START_UP, fw_startcommand());
end

function fw_facade:instance()
    if self._instance == nil then
        self._instance = fw_facade()
    end
    return self._instance
end

function fw_facade:register_command(cmd_name, cmd)
    self.controller:register_command(cmd_name, cmd)
end

function fw_facade:remove_command(cmd_name)
    self.controller:remove_command(cmd_name)
end

function fw_facade:has_command(cmd_name)
    return self.controller:has_command(cmd_name)
end

function fw_facade:send_msg_command(msg_name, body)
    local msg = fw_message(msg_name, body)
    self.controller:execute_command(msg)
end

function fw_facade:add_mgr(mgr_name, mgr)
    if self.managers[mgr_name] ~= nil then
        return self.managers[mgr_name]
    end

    self.managers[mgr_name] = mgr
    return mgr
end

function fw_facade:get_mgr(mgr_name)
    return self.managers[mgr_name]
end

function fw_facade:remove_mgr(args)
    if self.managers[mgr_name] ~= nil then
        self.managers[mgr_name] = nil
    end
end
