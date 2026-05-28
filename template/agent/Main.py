from fastapi import FastAPI
import uvicorn

app = FastAPI(title="Web4 Agent")

@app.get("/")
async def root():
    return {
        "status": "online",
        "agent": "LMLM Web4 Agent"
    }

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8080)
