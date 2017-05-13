import { IFileSystemItem } from "./file-system";

export interface IContent {
    downloadPath?:string;
    hash:string;
    isInProgress:boolean;
    currentFolder: string;
    parentFolder?: string;
    fsItems: IFileSystemItem[];
}