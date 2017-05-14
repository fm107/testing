export interface IFileSystemItem {
    id: number;
    downloadPath?: string;
    fullName: string;
    name: string;
    size: number;
    lastChanged: Date;
    type: string;   
}