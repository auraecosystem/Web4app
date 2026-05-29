from fastapi import FastAPI, WebSocket
from ws import websocket_handler

app = FastAPI(title="Web4 OS v1")

@app.get("/")
def root():
    return {"status": "Web4 OS running"}

@app.websocket("/ws")
async def ws_endpoint(ws: WebSocket):
    await websocket_handler(ws)
