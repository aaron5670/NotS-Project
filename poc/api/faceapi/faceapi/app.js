const tf = require('@tensorflow/tfjs-node')
const faceapi = require('@vladmandic/face-api');
const canvas = require('canvas');
const express = require('express');
const bodyParser = require('body-parser');
const routes = require('./routes/index');
const users = require('./routes/users');

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

    return res.send(results);
});


app.use('/', routes);
app.use('/users', users);

app.set('port', process.env.PORT || 3000);

const server = app.listen(app.get('port'), function () {
    console.log('Express server listening on port ' + server.address().port);
});

const setupFaceapi = async () => {
    desc = await loadLabeledImages();
    faceMatcher = new faceapi.FaceMatcher(desc, 0.6)
    console.log('Images data set loaded')
}

function loadLabeledImages() {
    const labels = ['Black Widow', 'Captain America', 'Captain Marvel', 'Hawkeye', 'Jim Rhodes', 'Thor', 'Mark Rutte', 'Tony Stark']
    return Promise.all(
        labels.map(async label => {
            const descriptions = []
            for (let i = 1; i <= 2; i++) {
                const img = await canvas.loadImage(`./images/${label}/${i}.jpg`)
                const detections = await faceapi.detectSingleFace(img).withFaceLandmarks().withFaceDescriptor()
                descriptions.push(detections.descriptor)
            }

            return new faceapi.LabeledFaceDescriptors(label, descriptions)
        })
    )
}

