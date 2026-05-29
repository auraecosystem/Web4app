def allow_event(user, event_type):
    role = user.get("role", "user")

    rules = {
        "user": ["chat", "ai_prompt", "tx_request"],
        "admin": ["*"],
        "agent": ["ai_response"]
    }

    allowed = rules.get(role, [])

    return "*" in allowed or event_type in allowed
