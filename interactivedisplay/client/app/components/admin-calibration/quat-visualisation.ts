import { ElementRef } from '@angular/core';
import * as THREE from 'three';

const AXIS_LENGTH = 430;
const TRACE_SEGMENTS = 25;

export class QuatVisualisation {

    private renderer: THREE.WebGLRenderer;
    private scene: THREE.Scene;
    private camera: THREE.Camera;
    private cameraPos = {x:0.425, y:0.595};
    // private rotationAxis = new THREE.Vector3(0,1,0);
    // private vectorQuaternion = new THREE.Quaternion();
    private meshTraceObject = new THREE.Mesh();
    private lineTraceObject = new THREE.Line();
    private vectorObject = new THREE.Line();



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
        this.renderer.render(this.scene, this.camera)

        this.turnCamera();
    }

    public destroy(): void {

    }

    public setQuat(x: number, y: number, z: number, w: number): void {
        // var theta = Math.acos(this.vectorQuaternion.w) * 2;
        // var sin = Math.sin(theta/2);
        // if (sin >= 0.01 || sin <= -0.01) {
        //     this.rotationAxis.x = this.vectorQuaternion.x / sin;
        //     this.rotationAxis.y = this.vectorQuaternion.y / sin;
        //     this.rotationAxis.z = this.vectorQuaternion.z / sin;
        //     this.rotationAxis.normalize();
        // }
        let vectorQuaternion = new THREE.Quaternion(x, y, z, w);
        vectorQuaternion.normalize();

        this.vectorObject.quaternion.w = vectorQuaternion.w;
        this.vectorObject.quaternion.x = vectorQuaternion.x;
        this.vectorObject.quaternion.y = vectorQuaternion.y;
        this.vectorObject.quaternion.z = vectorQuaternion.z;

        for (var i=1; i<= TRACE_SEGMENTS + 1; i++) {
            var currentQuat = new THREE.Quaternion().slerp(vectorQuaternion, (i-1) / TRACE_SEGMENTS);
            var currentVector = new THREE.Vector3(AXIS_LENGTH, 0, 0);
            currentVector.applyQuaternion(currentQuat);
            this.meshTraceObject.geometry.vertices[i].x = currentVector.x;
            this.meshTraceObject.geometry.vertices[i].y = currentVector.y;
            this.meshTraceObject.geometry.vertices[i].z = currentVector.z;
            this.lineTraceObject.geometry.vertices[i-1].x = currentVector.x;
            this.lineTraceObject.geometry.vertices[i-1].y = currentVector.y;
            this.lineTraceObject.geometry.vertices[i-1].z = currentVector.z;
        }
        this.meshTraceObject.geometry.verticesNeedUpdate = true;
        this.lineTraceObject.geometry.verticesNeedUpdate = true;

        // var rotAxisVec = new THREE.Vector3().copy(rotationAxis).multiplyScalar(AXIS_LENGTH);
        // this.rotationAxisObject.geometry.vertices[0].x = -rotAxisVec.x;
        // this.rotationAxisObject.geometry.vertices[0].y = -rotAxisVec.y;
        // this.rotationAxisObject.geometry.vertices[0].z = -rotAxisVec.z;
        // this.rotationAxisObject.geometry.vertices[1].x = rotAxisVec.x;
        // this.rotationAxisObject.geometry.vertices[1].y = rotAxisVec.y;
        // this.rotationAxisObject.geometry.vertices[1].z = rotAxisVec.z;
        // this.rotationAxisObject.geometry.verticesNeedUpdate = true;

        this.renderer.render(this.scene, this.camera)
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
        this.vectorObject = new THREE.Line(vectorGeom, vectorMat);
        this.scene.add(this.vectorObject);


        var meshTraceMat = new THREE.MeshBasicMaterial({color: 0x0066cc, side:THREE.DoubleSide, transparent: true, opacity: 0.05,});
        var lineTraceMat = new THREE.LineBasicMaterial({color: 0x0066cc});
        var meshTraceGeom = new THREE.Geometry();
        var lineTraceGeom = new THREE.Geometry();
        meshTraceGeom.vertices.push(new THREE.Vector3(0,0,0));
        for (var i=0; i<= TRACE_SEGMENTS; i++) {
            var currentQuat = new THREE.Quaternion().slerp(vectorQuaternion, i / TRACE_SEGMENTS);
            var currentVector = new THREE.Vector3(AXIS_LENGTH, 0, 0);
            currentVector.applyQuaternion(currentQuat);
            meshTraceGeom.vertices.push(currentVector);
            lineTraceGeom.vertices.push(currentVector);
        }
        for (var i=0; i <= TRACE_SEGMENTS; i++) {
            meshTraceGeom.faces.push(new THREE.Face3(0, i, i+1));
        }

        this.meshTraceObject = new THREE.Mesh(meshTraceGeom, meshTraceMat);
        this.lineTraceObject = new THREE.Line(lineTraceGeom, lineTraceMat);
        this.scene.add(this.meshTraceObject);
        this.scene.add(this.lineTraceObject);
    }


    public handlePointerMove(deltaX: number, deltaY: number) {
        this.cameraPos.x -= deltaX / 200;
        this.cameraPos.y += deltaY / 200;
        this.cameraPos.y = Math.min(this.cameraPos.y, 3.1415/2);
        this.cameraPos.y = Math.max(this.cameraPos.y, -3.1415/2);
        this.turnCamera();
    }


    private turnCamera() {
        this.camera.position.x = Math.sin(this.cameraPos.x) * 1000 * Math.cos(this.cameraPos.y);
        this.camera.position.z = Math.cos(this.cameraPos.x) * 1000 * Math.cos(this.cameraPos.y);
        this.camera.position.y = Math.sin(this.cameraPos.y) * 1000;
        this.camera.lookAt(new THREE.Vector3(0,0,0));
        this.renderer.render(this.scene, this.camera)
    }
}
