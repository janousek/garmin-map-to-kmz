/// <binding Clean='build' />

"use strict";

var gulp = require("gulp"),
	concat = require("gulp-concat"),
	cssmin = require("gulp-cssmin"),
	cache = require('gulp-cached'),
	minifyJs = require('gulp-uglify'),
	remember = require('gulp-remember'),
	uglify = require("gulp-uglify"),
	rebase = require("gulp-rebase-css-urls"),
	wrap = require("gulp-wrap"),
	plumber = require("gulp-plumber"),
	gutil = require("gulp-util");



gulp.task('build', function () {

	

	gulp.src('./node_modules/leaflet/dist/leaflet.css')
			.pipe(gulp.dest('wwwroot/libs/leaflet/'));


	gulp.src('./node_modules/leaflet/dist/images/*')
			.pipe(gulp.dest('wwwroot/libs/leaflet/images/'));


	gulp.src('./node_modules/leaflet/dist/leaflet.js')
			//.pipe(concat('webpack.bundle.aot.js'))
			.pipe(gulp.dest('wwwroot/libs/leaflet/'));



	gulp.src('./node_modules/jquery/dist/jquery.min.js')
			.pipe(gulp.dest('wwwroot/libs/'));


	gulp.src('./node_modules/jqueryui/jquery-ui.min.js')
			.pipe(gulp.dest('wwwroot/libs/'));


	gulp.src('./node_modules/jqueryui/jquery-ui.min.css')
			.pipe(gulp.dest('wwwroot/libs/'));



	gulp.src('./node_modules/jqueryui/images/*')
			.pipe(gulp.dest('wwwroot/libs/images/'));

	gulp.src('./node_modules/leaflet.gridlayer.googlemutant/Leaflet.GoogleMutant.js')
			.pipe(gulp.dest('wwwroot/libs/'));
	//
	
});