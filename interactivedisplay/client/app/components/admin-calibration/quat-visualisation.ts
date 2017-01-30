import { ElementRef } from '@angular/core';
import * as THREE from 'three';

const AXIS_LENGTH = 430;
const TRACE_SEGMENTS = 25;

export class QuatVisualisation {

    private renderer: THREE.WebGLRenderer;
    private scene: THREE.Scene;
    private camera: THREE.Camera;

    public constructor(element: ElementRef, width: number, height: number) {
        this.renderer = new THREE.WebGLRenderer({ antialias: true });
        element.nativeElement.appendChild(this.renderer.domElement);
        this.renderer.setSize(width, height);
        this.renderer.setClearColor('white');

        let aspectRatio = width / height;
        this.camera = new THREE.PerspectiveCamera(45, aspectRatio, 1, 10000);
        this.camera.position.set(700, 900, 120);
        this.camera.lookAt(new THREE.Vector3(0, 0, 0));

        this.scene = new THREE.Scene();

        // build reference coordinate system
        this.initGrid();
        this.initAxes();
        this.initVector();
        this.renderer.render(this.scene, this.camera)
    }

    public destroy(): void {

    }

    public setQuat(x: number, y: number, z: number, w: number): void {
    }


    private initAxes() {
        /* Adapted from http://quaternions.online/ */
        let xAxisMat = new THREE.LineBasicMaterial({color: 0xff0000, linewidth: 2});
        let xAxisGeom = new THREE.Geometry();
        xAxisGeom.vertices.push(new THREE.Vector3(0, 0, 0));
        xAxisGeom.vertices.push(new THREE.Vector3(AXIS_LENGTH, 0, 0));
        let xAxis = new THREE.Line(xAxisGeom, xAxisMat);
        this.scene.add(xAxis);

        let yAxisMat = new THREE.LineBasicMaterial({color: 0x00cc00, linewidth: 2});
        let yAxisGeom = new THREE.Geometry();
        yAxisGeom.vertices.push(new THREE.Vector3(0, 0, 0));
        yAxisGeom.vertices.push(new THREE.Vector3(0, AXIS_LENGTH, 0));
        let yAxis = new THREE.Line(yAxisGeom, yAxisMat);
        this.scene.add(yAxis);

        let zAxisMat = new THREE.LineBasicMaterial({color: 0x0000ff, linewidth: 2});
        let zAxisGeom = new THREE.Geometry();
        zAxisGeom.vertices.push(new THREE.Vector3(0, 0, 0));
        zAxisGeom.vertices.push(new THREE.Vector3(0, 0, AXIS_LENGTH));
        let zAxis = new THREE.Line(zAxisGeom, zAxisMat);
        this.scene.add(zAxis);
    }


    private initGrid() {
        /* Adapted from http://quaternions.online/ */
        let GRID_SEGMENT_COUNT = 5;
        let gridLineMat      = new THREE.LineBasicMaterial({color: 0xDDDDDD});
        let gridLineMatThick = new THREE.LineBasicMaterial({color: 0xAAAAAA, linewidth: 2});

        for (let i=-GRID_SEGMENT_COUNT; i<=GRID_SEGMENT_COUNT; i++) {
            let dist = AXIS_LENGTH * i / GRID_SEGMENT_COUNT;
            let gridLineGeomX = new THREE.Geometry();
            let gridLineGeomY = new THREE.Geometry();

            if (i == 0) {
                gridLineGeomX.vertices.push(new THREE.Vector3(dist, 0, -AXIS_LENGTH));
                gridLineGeomX.vertices.push(new THREE.Vector3(dist, 0,  0));

                gridLineGeomY.vertices.push(new THREE.Vector3(-AXIS_LENGTH, 0, dist));
                gridLineGeomY.vertices.push(new THREE.Vector3(           0, 0, dist));

                this.scene.add(new THREE.Line(gridLineGeomX, gridLineMatThick));
                this.scene.add(new THREE.Line(gridLineGeomY, gridLineMatThick));
            } else {
                gridLineGeomX.vertices.push(new THREE.Vector3(dist, 0, -AXIS_LENGTH));
                gridLineGeomX.vertices.push(new THREE.Vector3(dist, 0,  AXIS_LENGTH));

                gridLineGeomY.vertices.push(new THREE.Vector3(-AXIS_LENGTH, 0, dist));
                gridLineGeomY.vertices.push(new THREE.Vector3( AXIS_LENGTH, 0, dist));

                this.scene.add(new THREE.Line(gridLineGeomX, gridLineMat));
                this.scene.add(new THREE.Line(gridLineGeomY, gridLineMat));
            }
        }
    }

    private initVector() {
        /* Adapted from http://quaternions.online/ */
        let vectorMat = new THREE.LineBasicMaterial({color: 0x000000, linewidth: 3});
        let vectorGeom = new THREE.Geometry();
        vectorGeom.vertices.push(new THREE.Vector3(0, 0, 0));
        let vectorStandard = new THREE.Vector3(AXIS_LENGTH, 0, 0);
        let vectorStandardBack = new THREE.Vector3(-AXIS_LENGTH / 5, AXIS_LENGTH / 5, 0);
        vectorStandardBack.add(vectorStandard);
        let vectorQuaternion = new THREE.Quaternion();
        vectorStandard.applyQuaternion(vectorQuaternion);
        vectorStandardBack.applyQuaternion(vectorQuaternion);
        vectorGeom.vertices.push(vectorStandard);
        vectorGeom.vertices.push(vectorStandardBack);
        let vectorObject = new THREE.Line(vectorGeom, vectorMat);
        this.scene.add(vectorObject);
    }

}
