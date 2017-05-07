import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, ViewChild, OnInit } from '@angular/core';

import { TdDataTableService, TdDataTableSortingOrder, ITdDataTableSortChangeEvent, ITdDataTableColumn, TdSearchBoxComponent } from "@covalent/core";
import { ClickedItem } from "./ClickedItem";
import { IContent } from "../../model/content";

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
    @Input() data: any[];
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
        console.log(sortEvent);
        this.updateDataTable("sort");
    }

    search(searchTerm: string): void {
        this.searchTerm = searchTerm;
        this.updateDataTable("filter");
    }

    updateDataTable(action: string): void {
        const newData: any[] = this.data;
        switch (action) {
            case "filter":
                if (this.searchTerm) {
                    this.filteredData = this.dataTableService.filterData(newData, this.searchTerm, true);
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
                this.filteredData = this.dataTableService.sortData(newData, this.sortBy, this.sortOrder);
                break;
            default:
                this.filteredData = newData;
                break;
        }
    }

    private showFiles(item) {
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
        this.showFolder = true;
        const itemObj = new ClickedItem();
        itemObj.itemName = item;
        this.onItemClick.emit(itemObj);
    }

    private onClick(item) {
        this.showFolder = false;
        const itemObj = new ClickedItem();

        if (item.type) {
            itemObj.type = item.type;
            itemObj.itemName = item.fullName;
            this.onItemClick.emit(itemObj);
        }
    }
}
