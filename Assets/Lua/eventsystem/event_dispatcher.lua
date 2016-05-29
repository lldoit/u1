event_dispatcher = class()
function event_dispatcher:init()
    self.ui_event_manager = event_manager()
end

function event_dispatcher:instance()
    if self._instance == nil then
        self._instance = event_dispatcher()
    end

    return self._instance
end
