import json
from fastapi import WebSocket
from app.auth import decode_token
from core.policy import allow_event
from core.bus import publish, subscribe

async def websocket_handler(ws: WebSocket):
    await ws.accept()

    token = ws.headers.get("authorization")
    user = decode_token(token)

    if not user:
        await ws.close(code=4001)
        return

    await subscribe(f"user:{user['sub']}", ws)

    try:
        while True:
            raw = await ws.receive_text()
            event = json.loads(raw)

            if not allow_event(user, event["type"]):
                await ws.send_text(json.dumps({
                    "error": "not allowed"
                }))
                continue

            event["user"] = user["sub"]

            await publish("events", event)

    except:
        pass
