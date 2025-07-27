#!/bin/bash

echo "Starting Ollama container for development..."
docker compose up -d ollama

echo "Waiting for Ollama to be ready..."
sleep 10

echo "Pulling Qwen2.5:3B model..."
docker compose exec ollama ollama pull qwen2.5:3b

echo "Pulling deepseek-r1:1.5b model..."
docker compose exec ollama ollama pull deepseek-r1:1.5b

echo "Model pulled successfully!"