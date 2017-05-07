import { IFileSystemItem } from "./file-system";

export interface IContent {
    folder: IFileSystemItem;
    hash:string;
    isInProgress:boolean;
    currentFolder: string;
    parentFolder?: string;
    fsItems: IFileSystemItem[];
}