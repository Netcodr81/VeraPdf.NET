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
import org.verapdf.model.pdlayer.PDWatermarkAnnot;
import org.verapdf.pd.PDAnnotation;
import org.verapdf.pd.PDPage;

/**
 * Represents a watermark annotation, a specialized markup annotation for a watermark.
 *
 * <p>The subtype distinguishes watermark content from ordinary annotations so
 * conformance rules can validate its visibility and placement requirements. Standard
 * annotation data remains available through the inherited PD annotation model.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested type:
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Annotations.GFPDWatermarkAnnot}, implementing
 * {@code VeraPdf.Model.PDLayer.IPDWatermarkAnnot}; reuse {@code GFPDAnnot} and its
 * parser resource context.</p>
 *
 * @author Maxim Plushchov
 */
public class GFPDWatermarkAnnot extends GFPDAnnot implements PDWatermarkAnnot {

	/** Model type name for watermark annotations. */
	public static final String WATERMARK_ANNOTATION_TYPE = "PDWatermarkAnnot";

	public GFPDWatermarkAnnot(PDAnnotation annot, PDResourcesHandler pageResources, PDPage page) {
		super(annot, pageResources, page, WATERMARK_ANNOTATION_TYPE);
	}

}
