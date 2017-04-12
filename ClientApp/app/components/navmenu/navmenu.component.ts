import { Component, ViewChild, AfterViewInit } from "@angular/core";
import { UploadComponent } from "../uploadfile/upload.component";

import { TdFileService, IUploadOptions, TdFileUploadComponent } from "@covalent/file-upload";

import { NotificationsService, SimpleNotificationsComponent, PushNotificationsService } from "angular2-notifications";

@Component({
    selector: "nav-menu",
    templateUrl: "./navmenu.component.html",
    styleUrls: ["./navmenu.component.css"],
    providers: [TdFileService]
})
export class NavMenuComponent {

    constructor(
        private service: NotificationsService,
        private fileUploadService: TdFileService) {
    }

    selectEvent(file: File, uploadComponent: TdFileUploadComponent): void {
        console.log(file);
        if (file.type != "application/x-bittorrent") {
            uploadComponent.cancel();

            this.service.error("File Type Error",
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
            url: "api/Torrent/UploadFile",
            method: "post",
            file: file
        };

        this.fileUploadService.upload(options).subscribe((response) => {
            console.log(`this is response :${response}`);
        });

        this.service.success("File Uploaded",
            `${file.name} uploaded successfully`,
            {
                timeOut: 5000,
                showProgressBar: true,
                pauseOnHover: true,
                clickToClose: true,
                maxLength: 100
            });

        uploadComponent.cancel();
    };
}
