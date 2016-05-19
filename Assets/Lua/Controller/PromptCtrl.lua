require "View/PromptPanel"
require "Common/define"

require "3rd/pblua/login_pb"

PromptCtrl = {};
local this = PromptCtrl;

local panel;
local prompt;
local transform;
local gameObject;

--构建函数--
function PromptCtrl.New()
	logWarn("PromptCtrl.New--->>");
	return this;
end

function PromptCtrl.Awake()
	logWarn("PromptCtrl.Awake--->>");
	panelMgr:CreatePanel('Prompt', this.OnCreate);
end

--启动事件--
function PromptCtrl.OnCreate(obj)
	gameObject = obj;
	transform = obj.transform;

	panel = transform:GetComponent('UIPanel');
	prompt = transform:GetComponent('LuaBehaviour');
	logWarn("Start lua--->>"..gameObject.name);

	prompt:AddClick(PromptPanel.btnOpen, this.OnClick);
	resMgr:LoadPrefab('prompt', { 'PromptItem' }, this.InitPanel);
end

--初始化面板--
function PromptCtrl.InitPanel(objs)
	local count = 100; 
	local parent = PromptPanel.gridParent;
	for i = 1, count do
		local go = newObject(objs[0]);
		go.name = 'Item'..tostring(i);
		go.transform:SetParent(parent);
		go.transform.localScale = Vector3.one;
		go.transform.localPosition = Vector3.zero;
        prompt:AddClick(go, this.OnItemClick);

	    local label = go.transform:FindChild('Text');
	    label:GetComponent('Text').text = tostring(i);
	end
end

--滚动项单击--
function PromptCtrl.OnItemClick(go)
    log(go.name);
end

--单击事件--
function PromptCtrl.OnClick(go)
	if TestProtoType == ProtocalType.BINARY then
		this.TestSendBinary();
	end
	if TestProtoType == ProtocalType.PB_LUA then
		this.TestSendPblua();
	end
	if TestProtoType == ProtocalType.PBC then
    
	end
	if TestProtoType == ProtocalType.SPROTO then
    
	end
	logWarn("OnClick---->>>"..go.name);
end

--测试发送PBLUA--
function PromptCtrl.TestSendPblua()
    local login = login_pb.LoginRequest();
    login.id = 2000;
    login.name = 'game';
    login.email = 'jarjin@163.com';
    local msg = login:SerializeToString();
    ----------------------------------------------------------------
    local buffer = ByteBuffer.New();
    buffer:WriteShort(Protocal.Message);
    buffer:WriteByte(ProtocalType.PB_LUA);
    buffer:WriteBuffer(msg);
    networkMgr:SendMessage(buffer);
end

--测试发送二进制--
function PromptCtrl.TestSendBinary()
    local buffer = ByteBuffer.New();
    buffer:WriteShort(Protocal.Message);
    buffer:WriteByte(ProtocalType.BINARY);
    buffer:WriteString("ffff我的ffffQ靈uuu");
    buffer:WriteInt(200);
    networkMgr:SendMessage(buffer);
end

--关闭事件--
function PromptCtrl.Close()
	panelMgr:ClosePanel(CtrlNames.Prompt);
end