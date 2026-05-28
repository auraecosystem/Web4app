from copilot import CopilotClient
from copilot.session import PermissionHandler

session = await client.create_session(
    on_permission_request=PermissionHandler.approve_all,
    model="gpt-5",
)
