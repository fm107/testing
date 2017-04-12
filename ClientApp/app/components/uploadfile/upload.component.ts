import { Component, Output, Input } from '@angular/core'
import { Http } from '@angular/http';

import { Message } from 'primeng/primeng';

@Component({
    selector: 'upload',
    templateUrl: './upload.component.html',
    styleUrls: ['./upload.component.css']
})

export class UploadComponent {
    msgs: Message[];
    uploadedFiles: any[] = [];
    @Input() showDialog: boolean;

    constructor(private http: Http) {
        this.showDialog = false;
    }

    onUpload(event) {
        for (let file of event.files) {
            this.uploadedFiles.push(file);
        }

        this.msgs = [];
        this.msgs.push({ severity: 'info', summary: 'File Uploaded', detail: '' });
    }

    onclick() {
        this.http.get('/api/SampleData/WeatherForecasts').subscribe(result => {
            console.log(result);
        });
    }

    onVisibleChange(state: boolean) {
        this.showDialog = state;
    }
}