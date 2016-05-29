fw_message = class()

function fw_message:init(nm, bd)
    self.name = nm or "empty_name"
    self.body = bd
end
