/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AuthorDto } from './AuthorDto';
export type BookDto = {
    id?: string;
    title?: string | null;
    publicationYear?: number;
    tableOfContentsXml?: string | null;
    authors?: Array<AuthorDto> | null;
    createdAt?: string;
    updatedAt?: string | null;
    rowVersion?: string | null;
};

