import asyncio

from copilot import CopilotClient
from copilot.generated.session_events import (
    AssistantMessageData,
    AssistantMessageDeltaData,
    AssistantReasoningData,
    AssistantReasoningDeltaData,
    SessionIdleData,
)
from copilot.session import PermissionHandler

async def main():
    async with CopilotClient() as client:
        async with await client.create_session(
            on_permission_request=PermissionHandler.approve_all,
            model="gpt-5",
            streaming=True,
        ) as session:
            # Use asyncio.Event to wait for completion
            done = asyncio.Event()

            def on_event(event):
                match event.data:
                    case AssistantMessageDeltaData() as data:
                        # Streaming message chunk - print incrementally
                        delta = data.delta_content or ""
                        print(delta, end="", flush=True)
                    case AssistantReasoningDeltaData() as data:
                        # Streaming reasoning chunk (if model supports reasoning)
                        delta = data.delta_content or ""
                        print(delta, end="", flush=True)
                    case AssistantMessageData() as data:
                        # Final message - complete content
                        print("\n--- Final message ---")
                        print(data.content)
                    case AssistantReasoningData() as data:
                        # Final reasoning content (if model supports reasoning)
                        print("--- Reasoning ---")
                        print(data.content)
                    case SessionIdleData():
                        # Session finished processing
                        done.set()

            session.on(on_event)
            await session.send("Tell me a short story")
            await done.wait()  # Wait for streaming to complete

asyncio.run(main())
