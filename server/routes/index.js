/*jslint node: true todo: true nomen: true*/
'use strict';

var express = require('express'),
    router = express.Router();

router.get('/', function (req, res) {
    /*jslint unparam: true*/
    res.render('index', {
        title: 'Bare Bones Server'
    });
});

module.exports = router;