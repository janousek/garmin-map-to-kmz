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

    /*
    
    gulp.src('./AotCompilation/Superadmin/webpack.bundle.js')
			.pipe(concat('webpack.bundle.aot.js'))
			.pipe(gulp.dest(paths.webroot + 'app/superadmin/'));
    */
});