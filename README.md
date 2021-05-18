[![Build and deploy Patiënten API to Azure Web App](https://github.com/aaron5670/NotS-Project/actions/workflows/main_patients-api.yml/badge.svg?branch=main)](https://github.com/aaron5670/NotS-Project/actions/workflows/main_patients-api.yml)
# NotS-Project
NotS-Project with Augmented Reality and Facial Recognition.

## Gemaakte prototypes:
**Bekijk [hier](https://aaron5670.github.io/NotS-Project/) de prototypes.**

### Augmented Reality
- AR.js met A-Frame [demo](https://aaron5670.github.io/NotS-Project/prototype-1.html)
- Augmented Reality met Unity

### Face Recognition

#### Microsoft Azure Face
Bekijk [hier](https://github.com/aaron5670/NotS-Project/tree/main/opencv4nodejs-docker) de source code van OpenCV4Node.js + Express.js + Docker.

#### OpenCV4Node.js + Express.js + Docker
OpenCV4Node.js met Express.js prototype (Hierbij werkt alleen Face Detection). \
Bekijk [hier](https://github.com/aaron5670/NotS-Project/tree/main/opencv4nodejs-docker) de source code van OpenCV4Node.js + Express.js + Docker.
![Screenshot](https://github.com/aaron5670/NotS-Project/blob/main/docs/opencv4nodejs.png)

## Proof-of-Concept
Voor het proof of concept maken wij voor de backend gebruik van **Microservices**.

### Backend
- **Patiënten API**
  - Overview alle patiënten ([https://patients-api.azurewebsites.net/api/patients](https://patients-api.azurewebsites.net/api/patients))
  - Overview van een specifieke patiënten ([https://patients-api.azurewebsites.net/api/patients/1](https://patients-api.azurewebsites.net/api/patients/1))
  - Patient toevoegen gaat via een *POST* request:
    ```bash
    curl -H "Content-Type: application/json" --request POST --data '{
        "Name": "John Doe",
        "Address": "Straatnaam 4",
        "TelephoneNumber": 644337766,
        "BirthDate": "2010-10-10",
        "Gender": "Male",
        "BloodType": "C"
    }' https://patients-api.azurewebsites.net/api/patients
    ```
  - Een patiënt updaten gaat via een *PUT* request:
    ```bash
    curl -H "Content-Type: application/json" --request PUT --data '{
        "Name": "Jane Doe",
    }' https://patients-api.azurewebsites.net/api/patients/1
    ```
  - Een patiënt verwijderen gaat die een *DELETE* request:
    ```bash
    curl --request DELETE https://patients-api.azurewebsites.net/api/patients/1
    ```
