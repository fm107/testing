import { Component, Input, Output, EventEmitter } from "@angular/core";

import { IListItem } from "./ListItem";
import { ShoppingListService } from "./shopping-list.service";

@Component({
    selector: "shopping-list-item",
    templateUrl: "./shopping-list-item.component.html"
})

export class ShoppingListItemComponent {
    @Input() shopItem: IListItem;
    @Output() itemRemoved = new EventEmitter<IListItem>();

    constructor(private _shoppingListService: ShoppingListService) {

    }

    onDelete() {
        this._shoppingListService.deleteItem(this.shopItem);
        this.itemRemoved.emit(null);
    }
}