# Changelog
All notable changes to the VL.OpenCV repository will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

# [2.1.0] - 10/08/2021

## Added
* VideoIn "Video Acceleration" and "Hardware Device Index" input pins
* Facemark detector (thanks sebescudie!)
* CvImage type help patch
* Create an image from scratch help patch
* Intrisics help patch

## Changed
* Update OpenCVSharp dependency to version 4.5.3.20210725
* General cleanup

## Fixed
* Fix Resize (Scale) final dimensions smaller than 1x1 bug