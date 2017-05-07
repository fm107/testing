import { Component } from "@angular/core";

import { TdFileService, IUploadOptions, TdFileUploadComponent } from "@covalent/file-upload";
import { NotificationsService, SimpleNotificationsComponent, PushNotificationsService } from "angular2-notifications";

import { ContentService } from "../../services/content.service";
import {DataService} from "../../services/data.service";

@Component({
    selector: "upload-button",
    templateUrl: "./upload-button.component.html",
    styleUrls: ["./upload-button.component.css"],
    providers: [TdFileService]
})
export class UploadButtonComponent {

    constructor(private service: NotificationsService,
        private fileUploadService: TdFileService,
        private content: ContentService, private data: DataService) {
    }

    selectEvent(file: File, uploadComponent: TdFileUploadComponent): void {
        if (file.type != "application/x-bittorrent") {
            uploadComponent.cancel();

            this.service.error("File Error",
                `No torrents detected in given file`,
                {
                    timeOut: 5000,
                    showProgressBar: true,
                    pauseOnHover: true,
                    clickToClose: true,
                    maxLength: 100
                });
        } else {

            if (file.size > 1048576) {
                uploadComponent.cancel();

                this.service.error("File Size Exceeded",
                    `${file.name} exceeds the limit`,
                    {
                        timeOut: 5000,
                        showProgressBar: true,
                        pauseOnHover: true,
                        clickToClose: true,
                        maxLength: 100
                    });
            }
        }
    };

    uploadEvent(file: File, uploadComponent: TdFileUploadComponent) {
        const options: IUploadOptions = {
            url: `api/Torrent/UploadFile?folder=${this.content.currentFolder.getValue()}`,
            method: "post",
            file: file
        };

        this.fileUploadService.upload(options).subscribe(
            response => {
                this.service.success("File Uploaded",
                    `${file.name} uploaded successfully`,
                    {
                        timeOut: 5000,
                        showProgressBar: true,
                        pauseOnHover: true,
                        clickToClose: true,
                        maxLength: 100
                    });

                this.data.folderContent.next(JSON.stringify(response));
                this.content.getContent(this.content.currentFolder.getValue());
            },
            error => console.error(`Error while file uploading: ${error}`));

        uploadComponent.cancel();
    };
}
