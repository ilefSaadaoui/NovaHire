import requests
import json

url = "http://localhost:8000/generate-offer-letter"
payload = {
    "candidateName": "Test Candidate",
    "jobTitle": "Software Engineer",
    "salary": 3500.0,
    "currency": "DT",
    "startDate": "2026-06-01",
    "benefits": "Remote work, Health insurance",
    "language": "fr"
}

try:
    response = requests.post(url, json=payload)
    print(f"Status: {response.status_code}")
    print(f"Response: {response.text}")
except Exception as e:
    print(f"Error: {e}")
