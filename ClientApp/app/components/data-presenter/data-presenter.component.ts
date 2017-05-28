import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, ViewChild, OnInit } from '@angular/core';

import { TdDataTableService, TdDataTableSortingOrder, ITdDataTableSortChangeEvent, ITdDataTableColumn, TdSearchBoxComponent, TdDataTableColumnComponent } from "@covalent/core";
import { ClickedItem } from "./ClickedItem";
import { IContent } from "../../model/content";
import {IFileSystemItem} from "../../model/file-system";
import {isNumeric} from "rxjs/util/isNumeric"

declare var lity: any;

@Component({
    selector: 'data-presenter',
    templateUrl: './data-presenter.component.html',
    styleUrls: ['./data-presenter.component.css'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class DataPresenterComponent {
    showFolder = true;
    filteredData: any[];
    searchTerm: string;
    sortBy: string;
    hasData = true;
    sortOrder: TdDataTableSortingOrder;
    @Input() parentFolder: string;
    @Input() currentFolder: string;
    @Input() data: IContent[];
    @Output() onItemClick = new EventEmitter<ClickedItem>();

    @ViewChild('searchBox') searchBox: TdSearchBoxComponent;

    constructor(private dataTableService: TdDataTableService) {
    }

    private name: ITdDataTableColumn = {
        name: "name", label: "NAME #", tooltip: "Folder or file name", sortable: true
    }
    private size: ITdDataTableColumn = {
        name: "size", label: "SIZE", tooltip: "Folder or file size", sortable: true, numeric: true, format: v => v.toFixed(2)
    }
    private changed: ITdDataTableColumn = {
        name: "lastChanged", label: "LAST CHANGED", tooltip: "Folder or file last changed date", sortable: true, numeric: true
    }

    ngOnChanges(): void {
        this.updateDataTable(null);
        this.searchBox.value = "";
        this.hasData = true;
    }

    sort(sortEvent: ITdDataTableSortChangeEvent): void {
        this.sortBy = sortEvent.name;
        this.sortOrder = sortEvent.order === TdDataTableSortingOrder.Ascending ?
            TdDataTableSortingOrder.Descending : TdDataTableSortingOrder.Ascending;
        this.updateDataTable("sort");
    }

    search(searchTerm: string): void {
        this.searchTerm = searchTerm;
        this.updateDataTable("filter");
    }

    updateDataTable(action: string): void {
        const newData: IContent[] = this.data;

        switch (action) {
            case "filter":
                if (this.searchTerm) {
                    //this.filteredData = this.dataTableService.filterData(newData.map(i=>i.fsItems), this.searchTerm, true);
                    
                    this.filteredData = newData;
                    if (this.filteredData && this.filteredData.length > 0) {
                        this.hasData = true;
                    } else {
                        this.hasData = false;
                    }

                } else {
                    this.filteredData = newData;
                }
                break;
            case "sort":
                //this.filteredData = this.dataTableService.sortData(newData, this.sortBy, this.sortOrder);

                //newData.forEach(data=>this.sorting(data, this.sortBy, this.sortOrder));
                this.filteredData = this.sorting(newData, this.sortBy, this.sortOrder);
                //this.filteredData = newData;
                break;
            default:
                this.filteredData = newData;
                break;
        }
    }

    private sorting(data: IContent[], sortBy, sortOrder) {

        data.forEach((v, l)=> v.fsItems);
        for (var i = 0; i < data.length; i++) {
                data[i].fsItems.sort((a, b) => {
                    var compA = a[sortBy];
                    var compB = b[sortBy];
                    var direction = 0;
                    if (compA < compB) {
                        direction = -1;
                    }
                    else if (compA > compB) {
                        direction = 1;
                    }
                    return direction * (sortOrder === TdDataTableSortingOrder.Descending ? -1 : 1);
            });
        }

        return data;
        //return data.sort((a, b) => {
        //    var compA = a.fsItems[sortBy];
        //    var compB = b.fsItems[sortBy];
        //    var direction = 0;
        //    if (compA < compB) {
        //        direction = -1;
        //    }
        //    else if (compA > compB) {
        //        direction = 1;
        //    }
        //    return direction * (sortOrder === TdDataTableSortingOrder.Descending ? -1 : 1);
        //});
    }

    private showFiles1(item) {
        switch (item.type) {
        case "folder":
            if (this.showFolder) {
                return true;
            }
            return false;
        case "file":
            if (!this.showFolder) {
                return true;
            }
            return false;
        default:
            return false;
        }
    }

    private onUp(item) {
        console.log(item);
        setTimeout(() => {
            this.showFolder = true;
        }, 10);

        const itemObj = new ClickedItem();
        itemObj.folder = item;
        itemObj.showFiles = false;
        this.onItemClick.emit(itemObj);
    }

    private onClick(item: IFileSystemItem, content: IContent) {
        setTimeout(() => {
            this.showFolder = false;
        }, 10);

        const itemObj = new ClickedItem();

        if (item.type) {
            itemObj.showFiles = true;
            itemObj.hash = content.hash;
            itemObj.id = item.id;
            itemObj.type = item.type;
            itemObj.itemName = item.fullName;
            itemObj.folder = content.parentFolder;
            itemObj.downloadPath = item.downloadPath;
            itemObj.stream = item.stream;
            this.onItemClick.emit(itemObj);
        }
    }
}
