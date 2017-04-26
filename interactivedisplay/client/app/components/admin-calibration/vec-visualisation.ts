import { ElementRef } from '@angular/core';
import * as THREE from 'three';

const AXIS_LENGTH = 5;

export class VecVisualisation {

    private renderer: THREE.WebGLRenderer;
    private scene: THREE.Scene;
    private camera: THREE.Camera;
    private cameraPos = {x:0.425, y:0.595};

    private positionIndicator: THREE.Geometry;
    private positionBox: THREE.Mesh;

    public constructor(element: ElementRef, width: number, height: number) {
        this.renderer = new THREE.WebGLRenderer({ antialias: true });
        element.nativeElement.appendChild(this.renderer.domElement);
        this.renderer.setSize(width, height);
        this.renderer.setClearColor('white');

        let aspectRatio = width / height;
        this.camera = new THREE.PerspectiveCamera(45, aspectRatio, 1, 10000);
        this.camera.lookAt(new THREE.Vector3(0, 0, 0));

        this.scene = new THREE.Scene();

        // build reference coordinate system
        this.initGrid();
        this.initAxes();
        this.initVector();
        this.initCube();

        this.turnCamera();
    }

    public destroy(): void {

    }

    public setVector(x: number, y: number, z: number): void {
        this.positionBox.position.set(x * 100, y * 100, z * 100);
        this.positionIndicator.vertices.pop();
        this.positionIndicator.vertices.push(new THREE.Vector3(x, y, z));
        this.renderer.render(this.scene, this.camera);
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
        this.positionIndicator = new THREE.Geometry();
        this.positionIndicator.vertices.push(new THREE.Vector3(0, 0, 0));
        let endVector = new THREE.Vector3(5, 5, 5);
        this.positionIndicator.vertices.push(endVector);
        let vectorObject = new THREE.Line(this.positionIndicator, vectorMat);
        this.scene.add(vectorObject);
    }

    private initCube() {
        let meshMaterial = new THREE.MeshBasicMaterial({ color: 0xFF0000 });
        this.positionBox = new THREE.Mesh(new THREE.CubeGeometry(0.1, 0.1, 0.1), meshMaterial);
        this.scene.add(this.positionBox);
    }


    public handlePointerMove(deltaX: number, deltaY: number) {
        this.cameraPos.x -= deltaX / 200;
        this.cameraPos.y += deltaY / 200;
        this.cameraPos.y = Math.min(this.cameraPos.y, 3.1415/2);
        this.cameraPos.y = Math.max(this.cameraPos.y, -3.1415/2);
        this.turnCamera();
    }


    private turnCamera() {
        this.camera.position.x = Math.sin(this.cameraPos.x) * 4 * Math.cos(this.cameraPos.y);
        this.camera.position.z = Math.cos(this.cameraPos.x) * 4 * Math.cos(this.cameraPos.y);
        this.camera.position.y = Math.sin(this.cameraPos.y) * 4;
        this.camera.lookAt(new THREE.Vector3(0,0,0));
        this.renderer.render(this.scene, this.camera)
    }
}
