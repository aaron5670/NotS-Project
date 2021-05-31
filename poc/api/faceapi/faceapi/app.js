require('@tensorflow/tfjs-node')
const faceapi = require('@vladmandic/face-api');
const canvas = require('canvas');
const express = require('express');
const bodyParser = require('body-parser');
const fetch = require('node-fetch');
const fs = require('fs');
const path = require('path');

const app = express();
app.use(bodyParser.json({ limit: "10mb" }));

const { Canvas, Image, ImageData } = canvas;
faceapi.env.monkeyPatch({ Canvas, Image, ImageData });

let desc;
let faceMatcher;

Promise.all([
    faceapi.nets.faceRecognitionNet.loadFromDisk('./models'),
    faceapi.nets.faceLandmark68Net.loadFromDisk('./models'),
    faceapi.nets.ssdMobilenetv1.loadFromDisk('./models')
]).then(() => setupFaceapi())

app.post('/image', async (req, res) => {
    const {base64} = req.body;
    const i = await canvas.loadImage(`data:image/png;base64,${base64}`);
    const displaySize = { width: i.width, height: i.height }
    const detections = await faceapi.detectAllFaces(i).withFaceLandmarks().withFaceDescriptors()
    const resizedDetections = faceapi.resizeResults(detections, displaySize)
    const results = resizedDetections.map(d => faceMatcher.findBestMatch(d.descriptor))
    console.log('results\n', results)
    
    let patients = []

    results.forEach(r => {
        if (r._label !== "unknown") {

            console.log('Gevonden gezicht van: ' + r._label)
            patients.push(getPatientData(r._label))
        }
    })

    const patientsData = await Promise.all(patients)
    return res.json(JSON.parse(patientsData));
});

app.listen(process.env.PORT || 3000, function () {
    console.log('Express server listening on port ' + (process.env.PORT || 3000));
});

const getPatientData = (id) => {
    return fetch(`https://patients-api.azurewebsites.net/api/patients/${id}`)
        .then(res => res.text())
        .then(r => {
            console.log(r)
            return r;
        })
}

const setupFaceapi = async () => {
    desc = await loadLabeledImages();
    console.log(desc)
    faceMatcher = new faceapi.FaceMatcher(desc, 0.6)
    console.log('Images data set loaded')
}


function loadLabeledImages() {
    const dir = 'images'
    const labels = getDirectories(dir)
    return Promise.all(
        labels.map(async label => {
            const amount = fs.readdirSync(`${dir}/${label}`).length;
            const descriptions = []
            for (let i = 1; i <= amount; i++) {
                const img = await canvas.loadImage(`${dir}/${label}/${i}.jpg`)
                const detections = await faceapi.detectSingleFace(img).withFaceLandmarks().withFaceDescriptor()
                descriptions.push(detections.descriptor)
            }

            return new faceapi.LabeledFaceDescriptors(label, descriptions)
        })
    )
}

const getDirectories = srcPath => fs.readdirSync(srcPath).filter(file => fs.statSync(path.join(srcPath, file)).isDirectory())
