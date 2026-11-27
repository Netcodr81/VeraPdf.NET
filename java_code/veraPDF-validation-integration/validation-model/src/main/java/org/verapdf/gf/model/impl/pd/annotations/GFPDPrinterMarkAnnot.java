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
package org.verapdf.gf.model.impl.pd.annotations;

import org.verapdf.gf.model.impl.pd.GFPDAnnot;
import org.verapdf.pd.PDResourcesHandler;
import org.verapdf.model.pdlayer.PDPrinterMarkAnnot;
import org.verapdf.pd.PDAnnotation;
import org.verapdf.pd.PDPage;

/**
 * Models a printer-mark annotation used for production and press workflows.
 *
 * <p>Printer marks provide registration, color-bar, or similar print-production
 * information. These marks are specialized annotations rather than ordinary page
 * content; the type lets PDF/A profile rules identify and constrain them without
 * interpreting their visual appearance.</p>
 *
 * <p><b>.NET porting notes:</b> Implement
 * {@code VeraPdf.Model.PDLayer.IPDPrinterMarkAnnot} as
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Annotations.GFPDPrinterMarkAnnot}, reusing
 * {@code GFPDAnnot} and its parser-backed geometry/resource handling.</p>
 *
 * @author Maxim Plushchov
 */
public class GFPDPrinterMarkAnnot extends GFPDAnnot implements PDPrinterMarkAnnot {

	/** Model type name for printer-mark annotation rules. */
	public static final String PRINTER_MARK_ANNOTATION_TYPE = "PDPrinterMarkAnnot";

	public GFPDPrinterMarkAnnot(PDAnnotation annot, PDResourcesHandler pageResources, PDPage page) {
		super(annot, pageResources, page, PRINTER_MARK_ANNOTATION_TYPE);
	}

}
