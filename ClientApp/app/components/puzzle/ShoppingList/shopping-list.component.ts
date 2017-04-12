import { Component, OnInit } from "@angular/core";

import { IListItem } from "./ListItem";
import { ShoppingListService } from "./shopping-list.service";

@Component({
    selector: "shopping-list",
    templateUrl: "./shopping-list.component.html",
    providers:[ShoppingListService]
})
export class ShoppingListComponent implements OnInit {
    listItems: Array<IListItem>;
    selectedItem: IListItem;

    constructor(private _shoppingListService: ShoppingListService) {
    }

    ngOnInit(): void {
        this.listItems = this._shoppingListService.getItems();
    }

    onSelect(item: IListItem) {
        console.log("Selected item ", item);
        this.selectedItem = item;
    }

    onRemove() {
        this.selectedItem = null;
    }
}