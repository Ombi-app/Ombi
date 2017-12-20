import { IIssueCategory, IUser, RequestType } from "./";

export interface IIssues {
    id?: number;
    title: string;
    requestType: RequestType;
    providerId: string;
    subject: string;
    description: string;
    issueCategory: IIssueCategory;
    issueCategoryId: number;
    status: IssueStatus;
    resolvedDate?: Date;
    comments: IIssueComments[];
    requestId: number | undefined;
}

export enum IssueStatus {
    Pending = 0,
    InProgress = 1,
    Resolved = 2,
}

export interface IIssueComments {
    userId: string;
    comment: string;
    movieIssueId: number | undefined;
    tvIssueId: number | undefined;
    date: Date;
    user: IUser;
    issues: IIssues | undefined;
}

export interface IIssuesChat {
    comment: string;
    date: Date;
    username: string;
    adminComment: boolean;
}

export interface INewIssueComments {
    comment: string;
    issueId: number;
}

export interface IUpdateStatus {
    issueId: number;
    status: IssueStatus;
}
