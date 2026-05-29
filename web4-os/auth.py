import jwt

SECRET = "web4-os-secret"

def decode_token(token: str):
    if not token:
        return None

    try:
        return jwt.decode(token.replace("Bearer ", ""), SECRET, algorithms=["HS256"])
    except:
        return None
