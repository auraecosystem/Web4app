import json
from core.bus import redis

AGENT_MEMORY = {}

async def ai_agent_loop():
    pubsub = redis.pubsub()
    await pubsub.subscribe("events")

    async for msg in pubsub.listen():
        if msg["type"] != "message":
            continue

        event = json.loads(msg["data"])

        if event["type"] == "ai_prompt":

            user_id = event["user"]

            memory = AGENT_MEMORY.get(user_id, [])

            memory.append(event["data"])

            response = {
                "user": user_id,
                "type": "ai_response",
                "data": {
                    "text": f"[Web4 AI] processed: {event['data']}",
                    "memory_depth": len(memory)
                }
            }

            AGENT_MEMORY[user_id] = memory[-20:]

            await redis.publish(f"user:{user_id}", json.dumps(response))
