const debug = require('debug')('my express app');
const faceapi = require('face-api.js');
const canvas = require('canvas');
const express = require('express');
const logger = require('morgan');
const bodyParser = require('body-parser');
const routes = require('./routes/index');
const users = require('./routes/users');

const app = express();

const { Canvas, Image, ImageData } = canvas;
faceapi.env.monkeyPatch({ Canvas, Image, ImageData });

let desc;
let faceMatcher;

Promise.all([
    faceapi.nets.faceRecognitionNet.loadFromDisk('./models'),
    faceapi.nets.faceLandmark68Net.loadFromDisk('./models'),
    faceapi.nets.ssdMobilenetv1.loadFromDisk('./models')
]).then(() => setupFaceapi())

app.use(logger('dev'));
app.use(bodyParser.json({ limit: "50mb" }));

app.post('/image', async (req, res) => {
    const {base64} = req.body;

    const i = await canvas.loadImage(`data:image/png;base64,${base64}`);

    console.log('image uploaded')
    const displaySize = { width: i.width, height: i.height }
    const detections = await faceapi.detectAllFaces(i).withFaceLandmarks().withFaceDescriptors()
    const resizedDetections = faceapi.resizeResults(detections, displaySize)
    const results = resizedDetections.map(d => faceMatcher.findBestMatch(d.descriptor))

    return res.send(results);
});

const makeImage = async (image) => {
    return await canvas.loadImage(image)
}


app.use('/', routes);
app.use('/users', users);

// catch 404 and forward to error handler
app.use(function (req, res, next) {
    var err = new Error('Not Found');
    err.status = 404;
    next(err);
});

// error handlers

// development error handler
// will print stacktrace
if (app.get('env') === 'development') {
    app.use(function (err, req, res, next) {
        res.status(err.status || 500);
        res.render('error', {
            message: err.message,
            error: err
        });
    });
}

// production error handler
// no stacktraces leaked to user
app.use(function (err, req, res, next) {
    res.status(err.status || 500);
    res.render('error', {
        message: err.message,
        error: {}
    });
});

app.set('port', process.env.PORT || 3000);

var server = app.listen(app.get('port'), function () {
    debug('Express server listening on port ' + server.address().port);
});

const setupFaceapi = async () => {
    desc = await loadLabeledImages();
    faceMatcher = new faceapi.FaceMatcher(desc, 0.6)
    console.log(desc);
    console.log('$$$$$$$$$$$ Laden images met labels compleet')
}

function loadLabeledImages() {
    const labels = ['Black Widow', 'Captain America', 'Captain Marvel', 'Hawkeye', 'Jim Rhodes', 'Thor', 'Tony Stark']
    return Promise.all(
        labels.map(async label => {
            console.log(label)
            const descriptions = []
            for (let i = 1; i <= 2; i++) {
                //const img = await faceapi.fetchImage(`${API_URL}${label}/${i}.jpg`)
                const img = await canvas.loadImage(`./images/${label}/${i}.jpg`)
                console.log(img)
                const detections = await faceapi.detectSingleFace(img).withFaceLandmarks().withFaceDescriptor()
                descriptions.push(detections.descriptor)
            }

            return new faceapi.LabeledFaceDescriptors(label, descriptions)
        })
    )
}
