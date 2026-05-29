import json
from fastapi import WebSocket
from auth import decode_token
from policy import allow_event
from bus import publish, subscribe

async def websocket_handler(ws: WebSocket):
    await ws.accept()

    token = ws.headers.get("authorization")
    user = decode_token(token)

    if not user:
        await ws.close(code=4001)
        return

    user_id = user["sub"]

    await subscribe(f"user:{user_id}", ws)

    try:
        while True:
            raw = await ws.receive_text()
            event = json.loads(raw)

            if not allow_event(user, event.get("type")):
                await ws.send_text(json.dumps({
                    "error": "forbidden"
                }))
                continue

            event["user"] = user_id

            await publish(f"user:{user_id}", event)

    except Exception:
        pass
