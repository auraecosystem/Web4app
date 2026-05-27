# -----------------------------
# Project Automation Makefile
# -----------------------------

APP_NAME = app
PYTHON = python3
PIP = pip3

# Default target
all: help

# -----------------------------
# HELP
# -----------------------------
help:
	@echo "Available commands:"
	@echo "  make install     - Install dependencies"
	@echo "  make run         - Run main app"
	@echo "  make dev         - Run in development mode"
	@echo "  make test        - Run tests"
	@echo "  make clean       - Remove cache files"
	@echo "  make build       - Build project (if applicable)"
	@echo "  make docker      - Build Docker image"
	@echo "  make format      - Format code"

# -----------------------------
# INSTALL
# -----------------------------
install:
	$(PIP) install -r requirements.txt

# -----------------------------
# RUN
# -----------------------------
run:
	$(PYTHON) main.py

dev:
	uvicorn main:app --reload --host 0.0.0.0 --port 8000

# -----------------------------
# TEST
# -----------------------------
test:
	pytest -v

# -----------------------------
# CLEAN
# -----------------------------
clean:
	rm -rf __pycache__
	rm -rf .pytest_cache
	rm -rf *.pyc

# -----------------------------
# BUILD (extend for blockchain / Web4 builds)
# -----------------------------
build:
	@echo "Building project..."
	@echo "You can plug in: webpack / hardhat / go build here"

# -----------------------------
# DOCKER
# -----------------------------
docker:
	docker build -t $(APP_NAME) .

# -----------------------------
# FORMAT
# -----------------------------
format:
	black .
	isort .
