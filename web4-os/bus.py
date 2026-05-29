import json

SUBSCRIBERS = {}

async def publish(channel: str, event: dict):
    if channel in SUBSCRIBERS:
        for ws in SUBSCRIBERS[channel]:
            await ws.send_text(json.dumps(event))

async def subscribe(channel: str, websocket):
    if channel not in SUBSCRIBERS:
        SUBSCRIBERS[channel] = []

    SUBSCRIBERS[channel].append(websocket)
