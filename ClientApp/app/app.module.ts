import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { UniversalModule } from 'angular2-universal';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common'; 

import { AppComponent } from './components/app/app.component'
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { HomeComponent } from './components/home/home.component';
import { DialogComponent } from './components/dialog/dialog.component';
import { UploadButtonComponent } from "./components/upload-button/upload-button.component";

import { MaterialModule } from '@angular/material';

import { CovalentCoreModule } from '@covalent/core';

import { SimpleNotificationsModule } from 'angular2-notifications';

import { FileSizePipe } from './pipes/filesize.pipe';


@NgModule({
    bootstrap: [ AppComponent ],
    declarations: [
        AppComponent,
        NavMenuComponent,
        HomeComponent,
        UploadButtonComponent, DialogComponent, FileSizePipe
    ],
    imports: [
        UniversalModule, // Must be first import. This automatically imports BrowserModule, HttpModule, and JsonpModule too.
        FormsModule,
        CommonModule,

        MaterialModule,

        SimpleNotificationsModule.forRoot(),
        CovalentCoreModule,

        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent },
            { path: '**', redirectTo: 'home' }
        ])
    ]
})
export class AppModule {
}
