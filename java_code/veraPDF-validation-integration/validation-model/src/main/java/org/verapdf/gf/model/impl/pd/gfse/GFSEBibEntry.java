/*
 * This file is part of veraPDF Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.pd.gfse;

import org.verapdf.gf.model.impl.pd.GFPDStructElem;
import org.verapdf.model.selayer.SEBibEntry;
import org.verapdf.pd.structure.PDStructElem;
import org.verapdf.tools.TaggedPDFConstants;

/**
 * Represents a {@code BibEntry}, one bibliographic entry in a document's reference material.
 * <p>It is a typed view of a node in the tagged-PDF structure tree (ISO 32000, 14.7-14.8); the parser provides
 * the raw structure element and {@link GFSEFactory} chooses this adapter for semantic validation rules.</p>
 * <p><b>.NET porting notes:</b> use {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Gfse.GFSEBibEntry} with
 * {@code VeraPdf.Model.SELayer.ISEBibEntry}; the base structure adapter remains responsible for shared links.</p>
 */
public class GFSEBibEntry extends GFPDStructElem implements SEBibEntry {

    public static final String BIB_ENTRY_STRUCTURE_ELEMENT_TYPE = "SEBibEntry";

    public GFSEBibEntry(PDStructElem structElemDictionary) {
        super(structElemDictionary, TaggedPDFConstants.BIB_ENTRY, BIB_ENTRY_STRUCTURE_ELEMENT_TYPE);
    }
}
